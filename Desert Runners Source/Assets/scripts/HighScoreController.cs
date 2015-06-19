using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HighScoreController : MonoBehaviour
{
    public Text scoreText;
    
    private string secretKey = "atomerz09197046879"; // Edit this value and make sure it's the same as the one stored on the server
    private string addScoreURL = "http://narmgeek.ir/addscore.php?"; //be sure to add a ? to your url
    private string highscoreURL = "http://narmgeek.ir/display.php";
    
    private  string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);
        
        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
        
        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";
        
        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes [i], 16).PadLeft(2, '0');
        }
        
        return hashString.PadLeft(32, '0');
    }
    
    // remember to use StartCoroutine when calling this function!
    public IEnumerator PostScores(string name, int score)
    {
        scoreText.text = "Submitting hiscore...";
        //This connects to a server side php script that will add the name and score to a MySQL DB.
        // Supply it with a string representing the players name and the players score.
        string hash = Md5Sum(name + score + secretKey);
        
        string post_url = addScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash;
        
        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done
        
        if (hs_post.error != null)
        {
            print("There was an error posting the high score: " + hs_post.error);
            scoreText.text = "Submitting score failed.";
        }
        
        StartCoroutine(GetScores());
    }
    
    // Get the scores from the MySQL DB to display in a GUIText.
    // remember to use StartCoroutine when calling this function!
    public IEnumerator GetScores()
    {
        print(scoreText.name);
        scoreText.text = "Getting Hiscores...";
        WWW hs_get = new WWW(highscoreURL);
        yield return hs_get;
        
        if (hs_get.error != null)
        {
            print("There was an error getting the high score: " + hs_get.error);
            scoreText.text = "Getting hiscores failed.";
        } else
        {
            scoreText.text = hs_get.text; // this is a GUIText that will display the scores in game.
        }
    }
    
}