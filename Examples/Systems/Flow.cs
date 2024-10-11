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

        Delay.Wait(1, () =>
        {
            Console.Clear();
            Console.WriteLine(stateMachine.ToTree());
        }, true);

        while (Window.KeepOpen())
        {
            Time.Update();
            Delay.Update(Time.Delta);
            stateMachine.Update();
        }

        void TurnStart()
        {
            stateMachine.Get(Path.Running)?.Disable();
            stateMachine.Run(Attack);
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
            stateMachine.Run(Win);
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