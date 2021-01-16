/*
 *  SimpleHighlightFromBendCast 
 *  - Script that can be attached to an object displaying how to utilize 
 *  - the bend cast controllers events to react externally when an object
 *  - is highlighted or selected.
 *  
 *  Copyright(C) 2018  Ian Hanan
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

using UnityEngine;

public class SimpleHighlightFromBendcast : MonoBehaviour
{
    private Renderer _renderer;
    public BendCast bendcast;

    public Material highlightMaterial;
    private Material defaultMaterial;

    // Use this for initialization
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        defaultMaterial = _renderer.material;

        bendcast = FindObjectOfType<BendCast>();
        bendcast.onHover        .AddListener(Highlight);
        bendcast.onUnhover      .AddListener(Unhighlight);
        bendcast.onSelectObject .AddListener(PlaySelectSound);
    }

    void Highlight()
    {
        if (bendcast.highlightedObject == gameObject)
        {
            print("highlight");
            _renderer.material = highlightMaterial;
        }
    }

    void Unhighlight()
    {
        if (bendcast.highlightedObject == gameObject)
        {
            print("unhighlight");
            _renderer.material = defaultMaterial;
        }
    }

    void PlaySelectSound()
    {
        if (bendcast.selectedObject == gameObject)
        {
            GetComponent<AudioSource>().Play();
        }
    }
}