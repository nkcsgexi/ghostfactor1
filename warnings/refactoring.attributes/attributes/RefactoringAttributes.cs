using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    abstract class RefactoringAttributes : IRefactoringAttributes
    {
        private IList<IAttribute> attributes = new List<IAttribute>();

        public IAttribute getAttribute(AttributeKey key)
        {
            foreach (IAttribute attri in attributes)
            {
                if (attri.getKey().Equals(key))
                    return attri;
            }
            return new NullAttribute();
        }

        public AttributeKey[] getAllKeys()
        {
            IList<AttributeKey> keys = new List<AttributeKey>();
            foreach(IAttribute att in attributes)
            {
                keys.Add(att.getKey());
            }
            return keys.ToArray();
        }

        public IAttribute[] getAllAttributes()
        {
            return attributes.ToArray();
        }

        
        public void addAttribute(IAttribute att)
        {
            if(checkAttributeLegality(att))
            {
                attributes.Remove(att);
                attributes.Add(att);
            }        
        }

        public void removeAttribute(IAttribute att)
        {
            attributes.Remove(att);
        }

        public bool hasAttribute(AttributeKey key)
        {
            return !(getAttribute(key) is NullAttribute);
        }

        protected Boolean checkAttributeLegality(IAttribute a)
        {
            return a.getRefactoringType() == this.getRefactoringType();
        }

        public abstract RefactoringType getRefactoringType();
    }
}
