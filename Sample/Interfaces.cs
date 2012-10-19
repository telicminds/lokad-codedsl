using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Lokad
{
    public interface IIdentity {}

    public interface ICommand {}
    public interface IEvent {}


    public interface IUniverseCommand { }
    public interface IUniverseEvent<out TIdentity> : IEvent
        where TIdentity : IIdentity
    {
    }
}
