Simple Definitions  
------------------

**Namespaces**

Add namespace for our messages  
    namespace NameSpace

CS file
    namespace NameSpace  
    {  
    ...  
    }

Define data contract namespace

    extern "Lokad"

CS file  

    [DataContract(Namespace = "Lokad")]


**Shortcuts**  

For use interface in classes, need to create interface shortcut first, definition of interface IIdentity must be contained in C# file 
    
    if ! = IIdentity

For the next step define simple class with one property
 
    UniverseId!(long id)

Result

    [DataContract(Namespace = "Lokad")]
    public partial class UniverseId : IIdentity
    {
        [DataMember(Order = 1)] public long Id { get; private set; }
        
        UniverseId () {}
        public UniverseId (long id)
        {
            Id = id;
        }
    }

Method arguments shortcut, now we can use term `dateUtc` instead full definition with argument type and name.

    const dateUtc = DateTime dateUtc

Application service & state
---------------------------
Definition of application service must begining with interface key. 
interface Universe(UniverseId Id)
{
    // define shortcut for commands
    if ? = IUniverseCommand
    // define shortcut for events
    if ! = IUniverseEvent<UniverseId>

    CreateUniverse?(name)
        // override ToString() for command
        explicit "Create universe - {name}"
        UniverseCreated!(name)
        // override ToString() for event
        explicit "Universe {name} created"
}

Result  
    public interface IUniverseApplicationService
    {
        void When(CreateUniverse c);
    }

    public interface IUniverseState
    {
        void When(UniverseCreated e);
    }

Command and corresponding event
    [DataContract(Namespace = "Lokad")]
    public partial class CreateUniverse : IUniverseCommand
    {
        [DataMember(Order = 1)] public UniverseId Id { get; private set; }
        [DataMember(Order = 2)] public string Name { get; private set; }
        
        CreateUniverse () {}
        public CreateUniverse (UniverseId id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public override string ToString()
        {
            return string.Format(@"Create universe - {0}", Name);
        }
    }

    [DataContract(Namespace = "Lokad")]
    public partial class UniverseCreated : IUniverseEvent<UniverseId>
    {
        [DataMember(Order = 1)] public UniverseId Id { get; private set; }
        [DataMember(Order = 2)] public string Name { get; private set; }
        
        UniverseCreated () {}
        public UniverseCreated (UniverseId id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public override string ToString()
        {
            return string.Format(@"Universe {0} created", Name);
        }
    }

(Improved DSL Syntax for DDD and Event Sourcing) (http://abdullin.com/journal/2012/7/25/improved-dsl-syntax-for-ddd-and-event-sourcing.html)