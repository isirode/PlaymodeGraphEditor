using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Info : we need this because otherwise Unity will produce '{}'
// Warn : do not use properties, Unity will not serialize them
[Serializable]
public struct GraphDto
{
    public List<NodeDto> nodes;
    public List<ConnectionDto> connections;
}
 