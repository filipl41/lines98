using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
    private Color m_color;
    public BoardGenerator m_bg;
    private int m_x;
    private int m_y;
   
    public Ball(int i, int j, BoardGenerator bg, int x){
        m_x = i;
        m_y = j;
        m_bg = bg;
        setColor(x);
    }

    public Color getColor(){
        return m_color;
    }

    public void setColor(int x){
         if (x == 0)
            m_color = Color.black;
        else if (x == 1)
            m_color = Color.blue;
        else if (x == 2)
            m_color = Color.red;
        else if (x == 3)
            m_color = Color.green;
        else
            m_color = Color.yellow;
    }
    public void setColor(Color c){
        m_color = c;
    }
    public void setIndexes(int x, int y){
        m_x = x;
        m_y = y;
    }

    public int getX(){
        return m_x;
    }
    public int getY(){
        return m_y;
    }
    void OnMouseDown(){
        m_bg.setStartPoint(m_x, m_y);
        
    }
	

    
}
