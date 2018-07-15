using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {

	private int m_x;
	private int m_y;
	public BoardGenerator m_bg;
	public bool isObstacle = false;

	public Cell(int x, int y, BoardGenerator bg){
		m_x = x;
		m_y = y;
		m_bg = bg;
	}

	void OnMouseDown(){
        m_bg.setEndPoint(m_x, m_y);
    }

	public void setIndexes(int x, int y){
		m_x = x;
		m_y = y;
	}

	public void setBoardGenerator(BoardGenerator bg){
		m_bg = bg;
	}

	public int getX() {
		return m_x;
	}

	public int getY(){
		return m_y;
	}

}
