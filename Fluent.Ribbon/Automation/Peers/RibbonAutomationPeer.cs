namespace Fluent.Automation.Peers
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;
    using Fluent.Extensions;
    using JetBrains.Annotations;

    /// <summary>
    /// Automation peer for <see cref="Ribbon"/>.
    /// </summary>
    public class RibbonAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider
    {
        private Ribbon OwningRibbon => (Ribbon)this.Owner;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public RibbonAutomationPeer([NotNull] Ribbon owner)
            : base(owner)
        {
        }

        /// <inheritdoc />
        public override object GetPattern(PatternInterface patternInterface)
        {
            switch (patternInterface)
            {
                case PatternInterface.Scroll:
                {
                    ItemsControl ribbonTabHeaderItemsControl = this.OwningRibbon.TabControl;
                    if (ribbonTabHeaderItemsControl != null)
                    {
                        var automationPeer = CreatePeerForElement(ribbonTabHeaderItemsControl);
                        if (automationPeer != null)
                        {
                            return automationPeer.GetPattern(patternInterface);
                        }
                    }

                    break;
                }
            }

            return base.GetPattern(patternInterface);
        }

        /// <inheritdoc />
        protected override List<AutomationPeer> GetChildrenCore()
        {
            // If Ribbon is Collapsed, dont show anything in the UIA tree
            if (this.OwningRibbon.IsCollapsed)
            {
                return null;
            }

            var list = new List<AutomationPeer>();
            if (this.OwningRibbon.QuickAccessToolBar != null)
            {
                var automationPeer = CreatePeerForElement(this.OwningRibbon.QuickAccessToolBar);
                if (automationPeer != null)
                {
                    list.Add(automationPeer);
                }
            }

            if (this.OwningRibbon.TitleBar != null)
            {
                var automationPeer = CreatePeerForElement(this.OwningRibbon.TitleBar);
                if (automationPeer != null)
                {
                    list.Add(new RibbonTitleBarAutomationPeer(this.OwningRibbon.TitleBar));
                }
            }

            if (this.OwningRibbon.Menu != null)
            {
                var automationPeer = CreatePeerForElement(this.OwningRibbon.Menu);
                if (automationPeer != null)
                {
                    list.Add(automationPeer);
                }
            }

            if (this.OwningRibbon.TabControl != null)
            {
                CreatePeerForElement(this.OwningRibbon.TabControl)?.ForceEnsureChildren();
            }

            var childrenCore = base.GetChildrenCore();
            if (childrenCore != null && childrenCore.Count > 0)
            {
                list.AddRange(childrenCore);
                for (var i = 0; i < childrenCore.Count; i++)
                {
                    childrenCore[i].ForceEnsureChildren();
                }
            }

            var toolbarPanel = this.OwningRibbon.TabControl?.ToolbarPanel;
            if (toolbarPanel != null)
            {
                var automationPeer = CreatePeerForElement(toolbarPanel);
                if (automationPeer != null)
                {
                    list.Add(new FrameworkElementAutomationPeer(toolbarPanel));
                }
            }

            return list;
        }

        /// <inheritdoc/>
        protected override bool IsOffscreenCore()
        {
            return this.OwningRibbon.IsCollapsed 
                   || base.IsOffscreenCore();
        }

        /// <inheritdoc />
        public void Collapse()
        {
            this.OwningRibbon.IsMinimized = true;
        }

        /// <inheritdoc />
        public void Expand()
        {
            this.OwningRibbon.IsMinimized = false;
        }

        /// <inheritdoc />
        public ExpandCollapseState ExpandCollapseState => this.OwningRibbon.IsMinimized ? ExpandCollapseState.Collapsed : ExpandCollapseState.Expanded;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            this.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                                      oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                                      newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
        }
    }
}