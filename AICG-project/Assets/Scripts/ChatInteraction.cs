using UnityEngine;
using LLMUnity;
using UnityEngine.UI;

namespace AICG
{
    public class SimpleInteraction : MonoBehaviour
    {
        public LLMCharacter llmCharacter;
        public InputField playerText;
        public Text AIText;
        [SerializeField] private int currentMessage;

        void Start()
        {
            playerText.onSubmit.AddListener(onInputFieldSubmit);
            playerText.Select();
        }

        void onInputFieldSubmit(string message)
        {
            currentMessage++;
            playerText.interactable = false;
            if (currentMessage >= 2)
            {
                Ending(message);
            }
            else
            {
                SubmitMessage(message);
            }
            
        }

        void SubmitMessage(string message)
        {
            AIText.text = "...";
            _ = llmCharacter.Chat(message, SetAIText, AIReplyComplete);
        }

        void Ending(string message){
            llmCharacter.SetPrompt("The AI speaking as Alan Turing will now tell the player a score. This score is the percentage of how much the AI thinks the player is a human.", false);
            SubmitMessage(message);
        }

        public void SetAIText(string text)
        {
            AIText.text = text;
        }

        public void AIReplyComplete()
        {
            playerText.interactable = true;
            playerText.Select();
            playerText.text = "";
        }

        public void CancelRequests()
        {
            llmCharacter.CancelRequests();
            AIReplyComplete();
        }

        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }

        bool onValidateWarning = true;
        void OnValidate()
        {
            if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
                onValidateWarning = false;
            }
        }
    }
}
