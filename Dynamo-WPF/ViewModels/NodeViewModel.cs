using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Dynamo.UI.Models;
using ReactiveUI;

namespace Dynamo.UI.Wpf.ViewModels
{
    /// <summary>
    ///     Base ViewModel for all nodes.
    /// </summary>
    public class NodeViewModel : ReactiveObject
    {
        #region Design-Time Sample Data Utils
        [Obsolete]
        public NodeViewModel()
        {

        }

        [Obsolete("Use new NodeViewModel(Node)")]
        public Node Model
        {
            set { Initialize(value); }
        }
        #endregion

        public NodeViewModel(Node model)
        {
            Initialize(model);
        }

        private void Initialize(Node model)
        {
            InPorts = model.InPorts.CreateDerivedCollection(x => new PortViewModel(x));
            OutPorts = model.OutPorts.CreateDerivedCollection(x => new PortViewModel(x));
            nickName = model.NickNameChanged.ToProperty(this, x => x.NickName);
            description = model.DescriptionChanged.ToProperty(this, x => x.Description);
            argumentLacing = model.LacingChanged.ToProperty(this, x => x.ArgumentLacing);
        }

        /// <summary>
        ///     Displayed name of the node.
        /// </summary>
        public string NickName
        {
            get { return nickName.Value; }
        }
        private ObservableAsPropertyHelper<string> nickName;
        
        /// <summary>
        ///     Description of the node.
        /// </summary>
        public string Description
        {
            get { return description.Value; }
        }
        private ObservableAsPropertyHelper<string> description;

        /// <summary>
        ///     ViewModels for the node's input ports.
        /// </summary>
        public IReactiveDerivedList<PortViewModel> InPorts { get; private set; }

        /// <summary>
        ///     ViewModels for the node's output ports.
        /// </summary>
        public IReactiveDerivedList<PortViewModel> OutPorts { get; private set; }

        /// <summary>
        ///     Current strategy used to lace arguments to the node.
        /// </summary>
        public LacingStrategy ArgumentLacing
        {
            get { return argumentLacing.Value; }
        }
        private ObservableAsPropertyHelper<LacingStrategy> argumentLacing;

        /// <summary>
        ///     Determines if the node can be interacted with.
        /// </summary>
        public bool IsInteractionEnabled
        {
            get { return isInteractionEnabled; }
            set { this.RaiseAndSetIfChanged(ref isInteractionEnabled, value); }
        }
        private bool isInteractionEnabled;


        public IObservable<bool> NodeSelectedChanged;



        // TODO: NodeViewModel Missing Items
        // IsCustomFunction -- Boolean flag for whether or not the node is a custom node instance
        // IsVisible -- 3D Preview Toggle
        // IsUpstreamVisible -- 3D Preview Toggle for all upstream nodes
        // IsDisplayingLabels -- 3D Preview Toggle for labels

        /* Commands */

        // Rename -- Sets a new NickName
        // ShowHelp -- Displays the help dialog for a node
    }
}