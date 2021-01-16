/*
 *  Script to be attatched to controllers to allow the flashlight
 *  for the VR technique for the Flaslight to pick up objects as described
 *  for the flaslight for the HTC Vive.
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

public class FlashlightInteractor : MonoBehaviour
{
    Flashlight flashlight;

    private void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponentInParent<Flashlight>();

        flashlight.SetInteractor(transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        flashlight.AddCollidingObject(other);
    }

    private void OnTriggerStay(Collider other)
    {
        flashlight.AddCollidingObject(other);
    }

    private void OnTriggerExit(Collider other)
    {
        flashlight.RemoveCollidingObject(other);
    }
}
