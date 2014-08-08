using System;
using System.Reactive.Linq;
using ObservableExtensions;

namespace Dynamo.UI.Models
{
    /// <summary>
    ///     Interactive model of a Graph that supports asynchronous creation of connectors.
    /// </summary>
    /// <typeparam name="TNode">Type of all nodes.</typeparam>
    /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
    public class GraphModel<TNode, TMetaData>
    {
        /// <summary>
        ///     Observable sequence of new connectors
        /// </summary>
        public IObservable<Connector<TNode, TMetaData>> ConnectorCreatedStream
        {
            get;
            private set;
        }

        /// <summary>
        ///     Observable sequence of the start of a new connection action
        /// </summary>
        public IObservable<Connection<TNode, TMetaData>> BeginNewConnectionStream
        {
            get;
            private set;
        }

        /// <summary>
        ///     Observable sequence of the end of a new connection action
        /// </summary>
        public IObservable<Connection<TNode, TMetaData>?> EndNewConnectionStream
        {
            get;
            private set;
        }

        public GraphModel(
            IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
            IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream)
        {
            BeginNewConnectionStream = beginNewConnectionStream;
            EndNewConnectionStream = endNewConnectionStream;

            ConnectorCreatedStream =
                // Listen for a connection start event, then a connection end event
                BeginNewConnectionStream.FollowedBy(
                    EndNewConnectionStream, NewConnectorData.Create)
                    .Switch()                      // Only track newest connection
                    .Where(c => c.IsValid())       // Ignore cancelled connections
                    .Select(c => c.ToConnector()); // Build connector from data
        }

        private struct NewConnectorData
        {
            private Connection<TNode, TMetaData> start;
            private Connection<TNode, TMetaData>? end;

            public static NewConnectorData Create(
                Connection<TNode, TMetaData> start,
                Connection<TNode, TMetaData>? end)
            {
                return new NewConnectorData(start, end);
            }

            private NewConnectorData(
                Connection<TNode, TMetaData> start,
                Connection<TNode, TMetaData>? end)
                : this()
            {
                this.start = start;
                this.end = end;
            }

            public Connector<TNode, TMetaData> ToConnector()
            {
                return start.Type == ConnectionType.Output
                    // ReSharper disable PossibleInvalidOperationException
                    ? new Connector<TNode, TMetaData> { Start = start, End = end.Value }
                    : new Connector<TNode, TMetaData> { Start = end.Value, End = start };
                // ReSharper restore PossibleInvalidOperationException
            }

            public bool IsValid()
            {
                return end.HasValue && start.Type != end.Value.Type;
            }
        }
    }

    /// <summary>
    ///     Interactive model of a Graph that supports asynchronous creation and deletion of 
    ///     connectors.
    /// </summary>
    /// <typeparam name="TNode">Type of all nodes.</typeparam>
    /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
    public class GraphModelWithDeletion<TNode, TMetaData> : GraphModel<TNode, TMetaData>
    {
        /// <summary>
        ///     Observable sequence of deleted connectors
        /// </summary>
        public IObservable<Connector<TNode, TMetaData>> ConnectorDeletedStream
        {
            get;
            private set;
        }

        public GraphModelWithDeletion(
            IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
            IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream,
            IObservable<TNode> nodeDeletedStream,
            IObservable<Connector<TNode, TMetaData>> connectorDeletedStream)
            : base(beginNewConnectionStream, endNewConnectionStream)
        {
            // Observable sequence of "hanging" connectors
            var hangingConnectors =
                // For each new connector...
                ConnectorCreatedStream.SelectMany(
                    connector =>
                        nodeDeletedStream // ...listen for a node deletion...

                            // ...where the node deleted was attached to the connector.
                            .Where(
                                node => connector.Start.Node.Equals(node)
                                        || connector.End.Node.Equals(node))

                            .FirstAsync()           // Only listen for one occurence
                            .Select(_ => connector) // Produce the hanging connector.

                            // Stop listening once the connector has been deleted.
                            .TakeUntil(
                                connectorDeletedStream.Where(c => connector.Equals(c))));

            var validDeletions =
                // Make sure we only broadcast deletions of connectors that actually exist
                ConnectorCreatedStream.SelectMany(
                    connector =>
                        connectorDeletedStream.Where(
                            other => connector.Equals(other)));

            // Deleted connectors are both external deletions and hanging connectors caused by
            // node deletions.
            ConnectorDeletedStream = validDeletions.Merge(hangingConnectors);
        }
    }

    /// <summary>
    ///     Interactive model of a Graph that supports asynchronous creation, disconnection,
    ///     and reconnection of connectors.
    /// </summary>
    /// <typeparam name="TNode">Type of all nodes.</typeparam>
    /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
    public class GraphModelWithDisconnect<TNode, TMetaData>
        : GraphModelWithDeletion<TNode, TMetaData>
    {
        public GraphModelWithDisconnect(
            IObservable<Connector<TNode, TMetaData>> disconnectStream,
            IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
            IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream,
            IObservable<TNode> nodeDeletedStream)
            : base(
                //New connections are also triggered from disconnects
                beginNewConnectionStream.Merge(disconnectStream.Select(x => x.Start)),

                endNewConnectionStream,
                nodeDeletedStream,

                //Disconnects trigger deletions
                disconnectStream)
        { }
    }

    public class Workspace : GraphModelWithDisconnect<Node, int>
    {
        public Workspace(
            IObservable<Connector<Node, int>> disconnectStream,
            IObservable<Connection<Node, int>> beginNewConnectionStream,
            IObservable<Connection<Node, int>?> endNewConnectionStream, 
            IObservable<Node> nodeDeletedStream)
            : base(
                disconnectStream,
                beginNewConnectionStream,
                endNewConnectionStream,
                nodeDeletedStream)
        { }
    }
}