using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public interface IPredicateEvaluator
    {
        bool? Evaluate(string predicate, string[] parameters); // question mark after the return type means it can be returned as null
        // So in this case Evaluate can be true, false, or null
    }
}
