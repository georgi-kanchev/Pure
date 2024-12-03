using Pure.Engine.Execution;
using Path = Pure.Engine.Execution.StateMachine.Path;
using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Flow
{
    public static void Run()
    {
        var stateMachine = new StateMachine();
        stateMachine.Add(null, TurnStart, Move, Attack, TurnEnd);
        stateMachine.Add(TurnStart, ControlPlayer, ControlEnemy);
        stateMachine.Add(Attack, Miss, Hit, Kill);
        stateMachine.Add(Kill, Win, Lose);

        Time.CallAfter(1f, () =>
        {
            Console.Clear();
            Console.WriteLine(stateMachine.ToTree());
        }, true);

        while (Window.KeepOpen())
        {
            Time.Update();
            stateMachine.Update();
        }

        void TurnStart()
        {
            stateMachine.Get(Path.Running)?.Disable();
            stateMachine.GoTo(Attack);
        }

        void ControlPlayer()
        {
        }

        void ControlEnemy()
        {
        }

        void Move()
        {
        }

        void Attack()
        {
            stateMachine.GoTo(Win);
        }

        void Miss()
        {
        }

        void Hit()
        {
        }

        void Kill()
        {
        }

        void Win()
        {
            stateMachine.Get(Path.Parent, Path.Previous);
        }

        void Lose()
        {
        }

        void TurnEnd()
        {
        }
    }
}