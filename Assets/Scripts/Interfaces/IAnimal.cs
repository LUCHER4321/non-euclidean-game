public interface IIdler
{
    bool IdleToChase();
    bool IdleToHide();
    bool IdleToEat();
    bool IdleToEscape();
    bool IdleToReproduce();
    bool IdleToNull();
    void PreIdle();
    void Idle();
    void PostIdle();
}

public interface IHider
{
    bool HideToIdle();
    bool HideToChase();
    bool HideToEscape();
    void PreHide();
    void Hide();
    void PostHide();
}

public interface IChaser
{
    bool ChaseToIdle();
    bool ChaseToEscape();
    bool ChaseToFight();
    void PreChase();
    void Chase();
    void PostChase();
}

public interface IFighter
{
    bool FightToEscape();
    bool FightToNull();
    bool FightToEat();
    void PreFight();
    void Fight();
    void PostFight();
}

public interface IEater
{
    bool EatToIdle();
    void PreEat();
    void Eat();
    void PostEat();
}

public interface IEscaper
{
    bool EscapeToIdle();
    bool EscapeToFight();
    void PreEscape();
    void Escape();
    void PostEscape();
}

public interface IReproducer
{
    bool ReproduceToIdle();
    void PreReproduce();
    void Reproduce();
    void PostReproduce();
}
