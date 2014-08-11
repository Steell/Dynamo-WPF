﻿namespace Dynamo.UI.Wpf.ViewModels
{
    //TODO: Don't use inheritance, might force UI to have a reference to DynamoModels.dll
    //TODO: Make struct

    /// <summary>
    ///     Information describing a custom node creation action.
    /// </summary>
    public class NewCustomNodeEventArgs : Models.NewCustomNodeEventArgs
    {
        /// <summary>
        ///     Should this new custom node become the active workspace?
        /// </summary>
        public bool MakeActive { get; private set; }

        public NewCustomNodeEventArgs(
            string name, string category, string description="", bool makeActive=true)
            : base(name, category, description)
        {
            MakeActive = makeActive;
        }
    }

    /// <summary>
    ///     Information describing a home workspace creation actie.
    /// </summary>
    public class NewHomeWorkspaceEventArgs : Models.NewHomeWorkspaceEventArgs
    {
        public bool MakeActive { get; private set; }

        public NewHomeWorkspaceEventArgs(bool makeActive = true)
        {
            MakeActive = makeActive;
        }
    }
}