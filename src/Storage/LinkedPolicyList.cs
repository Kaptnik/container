using System;
using Unity.Policy;

namespace Unity.Storage
{
    public class LinkedPolicyList : LinkedNode<Type, object>, IPolicyList
    {
        public LinkedPolicyList(IUnityContainer container, LinkedNode<Type, object> parent)
        {
            Next = parent;
        }

        public void Clear(Type type, string name, Type policyInterface)
        {
            throw new NotImplementedException();
        }

        public void ClearAll()
        {
            throw new NotImplementedException();
        }

        public object Get(Type type, string name, Type policyInterface, out IPolicyList list)
        {
            throw new NotImplementedException();
        }

        public void Set(Type type, string name, Type policyInterface, object policy)
        {
            throw new NotImplementedException();
        }
    }
}
