using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace VE2
{
    /* 
     *  Cluster/link topology is kept on server. Cluster engine is server-to-client API for changing the graph,
     *  with special presentation layer on clients.
     *       
     * Play operations:
     *      -rebuild changed cluster (given IDs of included clusterables). During local play, we build final convex hull (FH) containing 
     *        all included clusterables. Then we build smooth border.   
     *      -link two linkables
     *      -unlink two linkables
     *      -move linkable => update adjacent links, don't rebuild cluster geometry
     *      -remove linkable => remove adjacent links, if it's in cluster, rebuild cluster
     *      -remove clusterable => if it's in cluster, rebuild cluster
     *           
     *  Local operations:
     *     -Moving clusterable and determining if its cluster membership changes (leaves or joins). 
     *     -Moving cluster is single operation where new cluster position is encoded. All clusterables are updated 
     *     by their relative position to cluster.     
     *     -Only position update is sent. Change in membership triggers topology request to server. 
     *      
     *  Distributed: 
     *      -Movement of clusterables is synchronized
     *      -When clusterable membership changes, initiator sends request to photon. Photon 
     *      forces other clients to rebuild changed cluster 
     *      -All adjacent links are removed before removing linkable
     *      -All linkables are created before creating links on them
     *      -Cluster is created after all its clusterables are created
     *  
     */
}
