using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using ObservableExtensions;

namespace Sandbox
{
    static class Program
    {
        /*
        public struct Port
        {
            public string Name;
        }

        public struct Node
        {
            public string Id;
            public Port[] InputPorts;
            public Port[] OutputPorts;

            public override string ToString()
            {
                return string.Format("(Node: Id={0})", Id);
            }
        }
         * */

        public struct Connector<TNode>
        {
            public Connection<TNode> Start, End;

            public override string ToString()
            {
                return string.Format("{0}=====>{1}", Start, End);
            }
        }

        public struct Connection<TNode>
        {
            public TNode Node;
            public int PortIndex;

            public override string ToString()
            {
                return Node.ToString(); //string.Format("(Connection: Node={0})", Node);//, PortIndex);
            }
        }

        public class WorkspaceModel<TNode>
        {
            public IObservable<Connector<TNode>> ConnectorCreatedStream { get; private set; }
            public IObservable<Connection<TNode>> BeginNewConnectionStream { get; private set; }
            public IObservable<Connection<TNode>?> EndNewConnectionStream { get; private set; }
            public IObservable<Connector<TNode>> ConnectorDeletedStream { get; private set; }

            public WorkspaceModel(
                IObservable<Connection<TNode>> beginNewConnectionStream, 
                IObservable<Connection<TNode>?> endNewConnectionStream,
                IObservable<TNode> nodeDeletedStream,
                IObservable<Connector<TNode>> connectorDeletedStream)
            {
                BeginNewConnectionStream = beginNewConnectionStream;
                EndNewConnectionStream = endNewConnectionStream;

                ConnectorCreatedStream = 
                    // Listen for one connection start event
                    BeginNewConnectionStream.FirstAsync()

                        // Then, listen for something to end the connection in progress.
                        .SelectMany(start => EndNewConnectionStream.Select(end => new { start, end }))

                        // Ignore cancelled connections
                        .Where(x => x.end.HasValue)

                        // Build connectors from the start and end
                        .Select(c => new Connector<TNode> { Start = c.start, End = c.end.Value })

                        // Repeat from the beginning.
                        .Repeat();

                var hangingConnectors =
                    ConnectorCreatedStream.SelectMany(
                        connector =>
                            nodeDeletedStream
                                .Where(
                                    node =>
                                        connector.Start.Node.Equals(node) || connector.End.Node.Equals(node))
                                .Select(_ => connector));

                ConnectorDeletedStream = connectorDeletedStream.Merge(hangingConnectors); //disconnects trigger deletion
            }
        }

        [Test]
        public static void CanCreateConnector()
        {
            var nodes =
                new[] {0, 1, 2}
                    .Select(x => x.ToString())
                    .ToArray();

            var startConnection = new Subject<Connection<string>>();
            var endConnection = new Subject<Connection<string>?>();
            var disconnect = new Subject<Connector<string>>();
            var deleteNode = new Subject<string>();

            startConnection.Dump("StartConnection");
            endConnection.Dump("EndConnection");
            disconnect.Dump("Disconnect");

            var workspace = new WorkspaceModel<string>(
                startConnection.Merge(disconnect.Select(connector => connector.Start)), //disconnects trigger a new connection
                endConnection,
                deleteNode,
                disconnect);

            workspace.ConnectorCreatedStream.Dump("  ConnectorCreated");
            workspace.BeginNewConnectionStream.Dump("  ConnectionStarted");
            workspace.ConnectorDeletedStream.Dump("  ConnectorDeleted");

            workspace.ConnectorCreatedStream.Take(2).Subscribe(disconnect.OnNext);
            workspace.ConnectorDeletedStream
                .Select(_ => new Connection<string> { Node = nodes[2] } as Connection<string>?)
                .Subscribe(endConnection.OnNext);

            startConnection.OnNext(new Connection<string> {Node = nodes[0]});
            startConnection.OnNext(new Connection<string> {Node = nodes[1]});
            endConnection.OnNext(new Connection<string> {Node = nodes[1]});

            Assert.Pass("Success");
        }
    }
}
