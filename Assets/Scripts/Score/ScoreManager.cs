using System;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>,
    IEventHandler<ValidatedChoiceEvent>
{
    public int Score { get; private set; } = 0;
    public int Lives { get; private set; } = 0;

    void Start()
    {
        this.Subscribe<ValidatedChoiceEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<ValidatedChoiceEvent>();
        base.OnDestroy();
    }

    void IEventHandler<ValidatedChoiceEvent>.Handle(ValidatedChoiceEvent validatedChoiceEvent)
    {
        if (validatedChoiceEvent.IsRight)
        {
            ++Score;
        }
        else
        {
            --Lives;
            
            if (Lives <= 0)
            {
                this.Publish(new GameOverEvent());
                return;
            }
        }
        
        this.Publish(new NewMonsterArrivesEvent());
    }
}