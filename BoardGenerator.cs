
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BoardGenerator : MonoBehaviour
{
    public GameObject canvas; //I use this to set the canvas after applying this script to an empty gameobject
    public GameObject panel; // I do the same for the panel

    public Vector2 ImageViewCount; // 6 * 4
    public Vector2 ImageViewSize; // 80 * 80
    public Vector2 InitialImageViewPosition; // -300 * 250
    public Vector2 ImageViewPositionOffset; // 125 * 200


    public GameObject[] GenerateImageView()
    {
		GameObject[] board = new GameObject[(int)(ImageViewCount.y * ImageViewCount.x)];
		int x = 0;
        for (int a = 0; a < ImageViewCount.y; a++)
        {
            for (int b = 0; b < ImageViewCount.x; b++)
            {
                board[x] = ImageViewBuilder(ImageViewSize, 
                new Vector2(InitialImageViewPosition.x + (ImageViewPositionOffset.x * b), 
                InitialImageViewPosition.y - (ImageViewPositionOffset.x * a)),
                panel.transform);
				x++;
            }
        }
		return board;
    }


    public GameObject ImageViewBuilder(Vector2 size, Vector2 position, Transform objectToSetImageView)
    {
        GameObject imageView = new GameObject("ImageView", typeof(RectTransform));
        RawImage image = imageView.AddComponent<RawImage>(); 
        //image.texture = Your Image Here
        RectTransform rectTransform = imageView.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;
        imageView.transform.SetParent(objectToSetImageView, false);
		return imageView;
    }

}
