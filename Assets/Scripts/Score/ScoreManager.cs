using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>,
    IEventHandler<ValidatedChoiceEvent>
{
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
            GameData.Score++;
            Logger.Log("Player chose correctly ! New score : " + GameData.Score);
            this.Publish(new ScoreChangedEvent(GameData.Score));
        }
        else
        {
            GameData.Lives--;
            Logger.Log("Player chose poorly ! New life count : " + GameData.Lives);
            this.Publish<PlayerLostLifeEvent>();
            
            if (GameData.Lives <= 0)
            {
                this.Publish<DeadEvent>();
                return;
            }
        }
        
        this.Publish<NewMonsterNeededEvent>();
    }
}