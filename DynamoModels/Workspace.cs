using System;

namespace Dynamo.UI.Models
{
    public class Workspace
    {
        public IObservable<Node> NewNodeStream;
        public IObservable<Node> DeletedNodeStream; 

        public IObservable<Connector> NewConnectorStream;
        public IObservable<Connector> DeletedConnectorStream;

        public IObservable<Note> NewNoteStream;
        public IObservable<Note> DeletedNoteStream;

        public string Filename { get; private set; }
    }
}