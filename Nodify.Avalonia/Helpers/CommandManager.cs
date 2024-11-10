using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nodify.Avalonia.Helpers
{
    public class CommandManager
    {
        public static void RegisterClassCommandBinding(IRoutedCommandBindable routedCommandBindable, RoutedCommandBinding commandBinding)
        {
            lock (((ICollection)routedCommandBindable.CommandBindings).SyncRoot)
            {
                if (routedCommandBindable.CommandBindings.Contains(commandBinding))
                {
                    throw new InvalidOperationException("Command binding already exists");
                }
                routedCommandBindable.CommandBindings.Add(commandBinding);
            }
        }
    }
}
