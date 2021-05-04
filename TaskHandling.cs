﻿using System;
using System.Threading.Tasks;
using System.Windows;

namespace WPFPages
{
	delegate void TaskToRun (Action action);
	class TaskHandling
	{
		// The argument type is shown originall as a response t a button as :
		// public async void RunTask(object vsender, EventArgs e);
		// or similar but my  function just accepts an action as its argument
		public async Task RunTask(Action task, AsyncCallback Callback)
		{
			await Task.Run (async () =>
			 {
				 // run the task itself here
				 task.BeginInvoke ( Callback, task );
			 });
//			return Task.CompletedTask;
		}
	}
}
