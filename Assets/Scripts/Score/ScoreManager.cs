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
            Debug.Log("Player chose correctly ! New score : " + GameData.Score);
            this.Publish(new ScoreChangedEvent(GameData.Score));
        }
        else
        {
            GameData.Lives--;
            Debug.Log("Player chose poorly ! New life count : " + GameData.Lives);
            this.Publish(new LifeCountChangedEvent(GameData.Lives));
            
            if (GameData.Lives <= 0)
            {
                this.Publish(new DeadEvent());
                return;
            }
        }
        
        this.Publish(new NewMonsterNeededEvent());
    }
}