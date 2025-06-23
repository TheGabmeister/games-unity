using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _typingSpeed = 0.05f; // Time between characters in seconds
    
    private string _fullText;
    private Coroutine _typingCoroutine;

    void Start()
    {
        // Store the full text but start with an empty display
        _fullText = "Hello World";
        _text.text = "";
        
        // Start the typewriter effect
        StartTypewriterEffect(_fullText);
    }
    
    public void StartTypewriterEffect(string textToType)
    {
        // Stop any existing coroutine first
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        
        _fullText = textToType;
        _typingCoroutine = StartCoroutine(TypewriterCoroutine());
    }
    
    private IEnumerator TypewriterCoroutine()
    {
        _text.text = "";
        
        // Reveal one character at a time
        for (int i = 0; i < _fullText.Length; i++)
        {
            _text.text = _fullText.Substring(0, i + 1);
            yield return new WaitForSeconds(_typingSpeed);
        }
        
        _typingCoroutine = null;
    }
    
    // Optional: Method to skip the typewriter effect and show the full text immediately
    public void SkipTypewriter()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        
        _text.text = _fullText;
    }

}
