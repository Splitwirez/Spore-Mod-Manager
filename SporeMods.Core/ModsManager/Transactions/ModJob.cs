    using SporeMods.Core.Mods;
using SporeMods.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SporeMods.Core.Mods
{
    public enum JobCategory
    {
        Install,
        Upgrade,
        Reconfigure,
        Uninstall,
        Repair
    }

    public class ModJob : JobBase
    {
        public ModJob(ITransaction transaction, IModText title, JobCategory category)
        {
            Transaction = transaction;
            Title = title;
            Category = category;
        }

        public ModJob(ITransaction transaction, IModText title, JobCategory category, IEnumerable<Exception> exceptions)
            : this(transaction, title, category)
        {
            if (exceptions != null)
            {
                foreach (Exception e in exceptions)
                {
                    Exceptions.Add(e);
                }
            }
        }


        IModText _title = null;
        public IModText Title
        {
            get => _title;
            internal set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }


        JobCategory _category = default(JobCategory);
        public JobCategory Category
        {
            get => _category;
            internal set
            {
                _category = value;
                NotifyPropertyChanged();
            }
        }
    }
}
