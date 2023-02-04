using HutongGames.PlayMaker;
using SFCore.Utils;
using System;
using System.Linq;

namespace Pet.Utils
{
    internal static class FsmUtils
    {
        public static void InsertMethodWithName(this FsmState state, Action method, int index, string name)
        {
            state.InsertMethod(method, index);
            state.Actions[index].Name = name;
        }

        public static void RemoveActionByName(this FsmState state, string name)
        {
            for (int i = 0; i < state.Actions.Length; i++)
            {
                if (state.Actions[i].Name == name)
                {
                    // Replace the specified action with an no-op action instead of remove it out right.
                    // This is so that the indices of the trailing actions remain the same.
                    // Otherwise, if this removal is called during a middle of a loop through of the actions, some of the trailing actions won't be executed.
                    state.Actions[i] = new NoopAction();
                    state.Actions[i].Init(state);
                    return;
                }
            }
            ModAssert.DebugBuild(false, $"Cannot find action named \"{name}\" in state \"{state.Name}\" (GO = \"{state.Fsm.GameObject.name}\", FSM = \"{state.Fsm.Name}\")");
        }

        public static int FindActionIndexByType(this FsmState state, Type actionType)
        {
            return state.Actions.Select((a, i) => new { a, i }).First(ai => ai.a.GetType() == actionType).i;
        }

        internal class InsertParam
        {
            // what to insert
            public Action Method { get; set; }
            public FsmStateAction Action { get; set; }
            // tag
            public string Named { get; set; }
            // index/delta
            public int At { get; set; }
            // ref
            public Type After { get; set; }
            public Type Before { get; set; }
        }
        public static FsmStateAction Insert(this FsmState state, InsertParam param)
        {
            ModAssert.AllBuilds(param.Before == null || param.After == null, "Before and After cannot be nonnull at the same time");
            ModAssert.AllBuilds(param.Method == null || param.Action == null, "Method and Action cannot be nonnull at the same time");
            ModAssert.AllBuilds(param.Method != null || param.Action != null, "Method and Action cannot be null at the same time");

            // index
            int index = 0;
            if (param.Before != null)
            {
                index = state.FindActionIndexByType(param.Before) - param.At;
            }
            else if (param.After != null)
            {
                index = state.FindActionIndexByType(param.After) + 1 + param.At;
            }
            else
            {
                index = param.At;
            }

            // insert
            if (param.Method != null)
            {
                state.InsertMethod(param.Method, index);
            }
            else if (param.Action != null)
            {
                state.InsertAction(param.Action, index);
            }
            else
            {
                throw new ModException("Should never arrive here");
            }

            // name
            if (param.Named != null)
            {
                state.Actions[index].Name = param.Named;
            }

            return state.Actions[index];
        }
    }

    internal class ShortCircuitProtectionAction : FsmStateAction
    {
        public override void OnUpdate()
        {
            base.Finish();
        }
    }

    internal class NoopAction : FsmStateAction
    {
        public override void OnEnter()
        {
            base.Finish();
        }
    }
}
