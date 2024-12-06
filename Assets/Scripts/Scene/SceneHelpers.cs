using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes
{
    public class SceneFader : MonoBehaviour
    {
        private static Transform GetOverlayCanvas()
        {
            var canvasArray = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvasArray)
            {
                if (canvas.CompareTag("FadeCanvas"))
                {
                    return canvas.transform;
                }
            }
            var tmpCanvas = new GameObject("fadeCanvas").AddComponent<Canvas>();
            tmpCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tmpCanvas.tag = "FadeCanvas";
            tmpCanvas.sortingOrder = 4;
            return tmpCanvas.transform;
        }

        public static GameObject GetFadingImage()
        {
            var go = new GameObject("SceneFade");
            go.transform.SetParent(GetOverlayCanvas());
            var img = go.AddComponent<Image>();
            var rt = img.rectTransform;
            rt.sizeDelta = new Vector2(Screen.width, Screen.height);
            rt.anchoredPosition = Vector2.zero;
            return go;
        }

        public static void FadeToScene(string name, Color color)
        {
            var fade = GetFadingImage().AddComponent<SceneHelpers>();
            fade.StartCoroutine(fade.FadeIn(name, color));
        }

        public static void FadeOut(Color color)
        {
            var fade = GetFadingImage().AddComponent<SceneHelpers>();
            fade.StartCoroutine(fade.FadeOut(color));
        }

        public static void StopEditor()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
    
    
    public class SceneHelpers : MonoBehaviour
    {
        [SerializeField] private float fadeSpeed = 1.5f;
        [SerializeField] private float fadeOutDelay = 0.5f;
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public IEnumerator FadeIn(string sceneName, Color color)
        {
            Color tmpColor = color;
            tmpColor.a = 0;
            image.color = tmpColor;
            while (image.color.a < 1)
            {
                tmpColor = image.color;
                tmpColor.a += Time.deltaTime * fadeSpeed;
                image.color = tmpColor;
                yield return new WaitForEndOfFrame();
            }
            
            SceneManager.LoadScene(sceneName);
        }

        public IEnumerator FadeOut(Color color)
        {
            var tmpColor = color;
            tmpColor.a = 1;
            image.color = tmpColor;
            
            yield return new WaitForSeconds(fadeOutDelay);
            
            while (image.color.a > 0)
            {
                tmpColor = image.color;
                tmpColor.a -= Time.deltaTime * fadeSpeed;
                image.color = tmpColor;
                yield return new WaitForEndOfFrame();
            }
            Destroy(image.gameObject);
        }
    }
}