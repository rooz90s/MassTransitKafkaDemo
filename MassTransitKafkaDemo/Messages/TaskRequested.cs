// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.10.0.0
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------

using SolTechnology.Avro;

namespace MassTransitKafkaDemo.Messages
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Avro;
	using Avro.Specific;

	/// <summary>
	/// The event raised when a task is requested.
	/// </summary>
	public partial class TaskRequested : ISpecificRecord
	{
		public static Schema _SCHEMA =  Avro.Schema.Parse(@"{""type"":""record"",""name"":""TaskRequested"",""namespace"":""MassTransitKafkaDemo.Messages"",""fields"":[{""name"":""Id"",""doc"":""The id of the task"",""type"":{""type"":""string"",""logicalType"":""uuid""}},{""name"":""RequestedDate"",""doc"":""The date when the task was requested"",""type"":{""type"":""long"",""logicalType"":""timestamp-millis""}},{""name"":""RequestedBy"",""doc"":""The name of the user that requested the task"",""type"":""string""}]}");
		/// <summary>
		/// The id of the task
		/// </summary>
		private System.Guid _Id;
		/// <summary>
		/// The date when the task was requested
		/// </summary>
		private System.DateTime _RequestedDate;
		/// <summary>
		/// The name of the user that requested the task
		/// </summary>
		private string _RequestedBy;
		public virtual Schema Schema
		{
			get
			{
				return TaskRequested._SCHEMA;
			}
		}
		/// <summary>
		/// The id of the task
		/// </summary>
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				this._Id = value;
			}
		}
		/// <summary>
		/// The date when the task was requested
		/// </summary>
		public System.DateTime RequestedDate
		{
			get
			{
				return this._RequestedDate;
			}
			set
			{
				this._RequestedDate = value;
			}
		}
		/// <summary>
		/// The name of the user that requested the task
		/// </summary>
		public string RequestedBy
		{
			get
			{
				return this._RequestedBy;
			}
			set
			{
				this._RequestedBy = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.Id;
			case 1: return this.RequestedDate;
			case 2: return this.RequestedBy;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.Id = (System.Guid)fieldValue; break;
			case 1: this.RequestedDate = (System.DateTime)fieldValue; break;
			case 2: this.RequestedBy = (System.String)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
