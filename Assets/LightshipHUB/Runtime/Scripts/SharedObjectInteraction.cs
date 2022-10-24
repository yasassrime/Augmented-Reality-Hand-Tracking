using System;
using UnityEngine;
using UnityEngine.Events;

namespace Niantic.ARDK.Templates 
{
	public class SharedObjectInteraction : MonoBehaviour 
	{	
		[Serializable]
    public class AREvent : UnityEvent {}
		public AREvent OnTap = new AREvent();
		public AREvent OnDistance = new AREvent();

		internal void AnimateObjectTap() 
		{
			OnTap.Invoke();
		}

		internal void AnimateObjectDistance() 
		{
			OnDistance.Invoke();
		}
	}
}
