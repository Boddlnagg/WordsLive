using System;
using System.ComponentModel;
using System.Windows.Media;

namespace Words.MediaOrderList
{
	public class ActivatableItemContainer<T> : INotifyPropertyChanged where T : IActivatable
	{
		ActivatableBindingList<T> parent;
		T data;
		
		public void NotifyActivatedChanged()
		{
			OnPropertyChanged(new PropertyChangedEventArgs("IsActive"));
		}
		
		public ActivatableItemContainer(ActivatableBindingList<T> parent, T data)
		{
			this.parent = parent;
			if (data is INotifyPropertyChanged)
			{
				((INotifyPropertyChanged)data).PropertyChanged += new PropertyChangedEventHandler(DataPropertyChanged);
			}
			this.data = data;
		}
		
		public T Data
		{
			get
			{
				return this.data;
			}
		}

		void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				OnPropertyChanged(e);
			}
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null) {
				PropertyChanged(this, e);
			}
		}
		
		public bool IsActive
		{
			get
			{
				return parent.ActiveItem == this;
			}
		}
	}
}
