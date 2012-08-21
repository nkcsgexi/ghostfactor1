using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.resources;

namespace warnings.ui
{
    class RefactoringCodeAction : ICodeAction
    {
        public ICodeActionEdit GetEdit(CancellationToken cancellationToken = new CancellationToken())
        {
            return new RefactoringCodeEdit();
        }

        public ImageSource Icon
        {
            get { return ResourcePool.getIcon(); }
        }

        public string Description
        {
            get { return "Potential Manual Refactoring Error."; }
        }
    }

    class RefactoringCodeEdit : ICodeActionEdit
    {
        public Task ApplyAsync(IWorkspace workspce, CancellationToken cancellationToken = new CancellationToken())
        {
            return new Task(new Action(doNothing));
        }

        public object GetPreview(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        private void doNothing()
        {
            return;
        }
    }

}
