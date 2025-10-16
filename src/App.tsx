import { useState, useEffect } from 'react';
import { FileList, FileItem } from './components/FileList';
import { FileDetails } from './components/FileDetails';
import { ErrorPanel } from './components/ErrorPanel';
import { Button } from './components/ui/button';
import { Play, Square, Wrench, Settings, FolderOpen, AlertTriangle, HelpCircle } from 'lucide-react';
import { toast } from 'sonner@2.0.3';
import { Toaster } from './components/ui/sonner';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from './components/ui/dropdown-menu';

// Mock file data
const mockFiles: FileItem[] = [
  {
    id: '1',
    name: 'config.xml',
    extension: 'xml',
    description: 'Configuration file containing application settings and parameters.',
    size: '2.4 KB',
    modified: '2025-10-08 14:30'
  },
  {
    id: '2',
    name: 'data.json',
    extension: 'json',
    description: 'JSON data file with user preferences and application state.',
    size: '5.1 KB',
    modified: '2025-10-08 12:15'
  },
  {
    id: '3',
    name: 'schema.xml',
    extension: 'xml',
    description: 'XML schema definition for data validation and structure.',
    size: '3.8 KB',
    modified: '2025-10-07 16:45'
  },
  {
    id: '4',
    name: 'settings.json',
    extension: 'json',
    description: 'Application settings stored in JSON format.',
    size: '1.2 KB',
    modified: '2025-10-07 09:20'
  },
  {
    id: '5',
    name: 'declaration.xml',
    extension: 'xml',
    description: 'Declaration file defining application components and dependencies.',
    size: '4.5 KB',
    modified: '2025-10-06 11:00'
  },
  {
    id: '6',
    name: 'manifest.json',
    extension: 'json',
    description: 'Manifest file containing metadata and configuration.',
    size: '2.9 KB',
    modified: '2025-10-05 15:30'
  }
];

export default function App() {
  const [files, setFiles] = useState<FileItem[]>(mockFiles);
  const [selectedFiles, setSelectedFiles] = useState<Set<string>>(new Set());
  const [activeFileId, setActiveFileId] = useState<string | null>(null);
  const [isExecuting, setIsExecuting] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [isDeclarationMode, setIsDeclarationMode] = useState(false);
  const [progress, setProgress] = useState(0);

  const activeFile = files.find(f => f.id === activeFileId) || null;

  // Simulate progress when executing
  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (isExecuting) {
      setProgress(0);
      interval = setInterval(() => {
        setProgress(prev => {
          if (prev >= 100) {
            clearInterval(interval);
            return 100;
          }
          return prev + 1;
        });
      }, 100);
    } else {
      setProgress(0);
    }
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [isExecuting]);

  const handleCheckboxToggle = (fileId: string) => {
    const newSelected = new Set(selectedFiles);
    if (newSelected.has(fileId)) {
      newSelected.delete(fileId);
    } else {
      newSelected.add(fileId);
    }
    setSelectedFiles(newSelected);
  };

  const handleDelete = () => {
    if (selectedFiles.size === 0) {
      toast.error('No files selected');
      return;
    }

    const filesToDelete = Array.from(selectedFiles);
    const deletedFileNames = files
      .filter(f => filesToDelete.includes(f.id))
      .map(f => f.name)
      .join(', ');

    setFiles(files.filter(f => !selectedFiles.has(f.id)));
    
    if (activeFileId && selectedFiles.has(activeFileId)) {
      setActiveFileId(null);
    }
    
    setSelectedFiles(new Set());
    toast.success(`Deleted: ${deletedFileNames}`);
  };

  const toggleDeclaration = () => {
    setIsDeclarationMode(!isDeclarationMode);
    toast.info(isDeclarationMode ? 'Switched to Engineering mode' : 'Switched to Declaration mode');
  };

  const handleJobEditor = () => {
    toast.info('Opening Job Editor...');
  };

  const handleGraph = () => {
    toast.info('Opening Graph...');
  };

  const handleOpenLog = () => {
    toast.info('Opening Log...');
  };

  const handleSettings = () => {
    toast.info('Opening settings...');
  };

  const handleUserManual = () => {
    toast.info('Opening User Manual...');
  };

  const handleReleaseNotes = () => {
    toast.info('Opening Release Notes...');
  };

  const handleReportIssue = () => {
    toast.info('Redirecting to code.europa.eu...');
  };

  const handleAbout = () => {
    toast.info('About Vecto');
  };

  const toggleExecution = () => {
    if (!isExecuting) {
      // Start execution
      setIsExecuting(true);
      toast.success('Execution started');
      
      // Simulate an error after 2 seconds
      setTimeout(() => {
        setErrors(prev => [...prev, `Error executing ${activeFile?.name || 'file'}: Simulated execution error at ${new Date().toLocaleTimeString()}`]);
      }, 2000);
    } else {
      // Stop execution
      setIsExecuting(false);
      toast.info('Execution stopped');
    }
  };

  const clearError = (index: number) => {
    setErrors(errors.filter((_, i) => i !== index));
  };

  const handleMoveUp = () => {
    if (selectedFiles.size !== 1) {
      toast.error('Please select exactly one file to move');
      return;
    }
    
    const fileId = Array.from(selectedFiles)[0];
    const index = files.findIndex(f => f.id === fileId);
    
    if (index > 0) {
      const newFiles = [...files];
      [newFiles[index - 1], newFiles[index]] = [newFiles[index], newFiles[index - 1]];
      setFiles(newFiles);
      toast.success('File moved up');
    }
  };

  const handleMoveDown = () => {
    if (selectedFiles.size !== 1) {
      toast.error('Please select exactly one file to move');
      return;
    }
    
    const fileId = Array.from(selectedFiles)[0];
    const index = files.findIndex(f => f.id === fileId);
    
    if (index < files.length - 1) {
      const newFiles = [...files];
      [newFiles[index], newFiles[index + 1]] = [newFiles[index + 1], newFiles[index]];
      setFiles(newFiles);
      toast.success('File moved down');
    }
  };

  return (
    <div className="min-h-screen bg-white p-4">
      <div className="h-[calc(100vh-2rem)] flex flex-col gap-4">
        {/* Top toolbar */}
        <div className="bg-gray-100 border border-gray-300 rounded p-3 flex items-center justify-between">
          <div className="flex items-center gap-2">
            {/* Open File button */}
            <Button
              variant="outline"
              className="bg-white hover:bg-gray-50 border-gray-300 rounded gap-1"
            >
              <FolderOpen className="h-4 w-4" />
              Open File
            </Button>

            {/* Play/Stop buttons */}
            <Button
              size="icon"
              className="rounded bg-green-500 hover:bg-green-600 border-0"
              onClick={toggleExecution}
              disabled={isExecuting}
            >
              <Play className="h-4 w-4 text-white" />
            </Button>
            
            <Button
              size="icon"
              className="rounded bg-red-500 hover:bg-red-600 border-0"
              onClick={toggleExecution}
              disabled={!isExecuting}
            >
              <Square className="h-4 w-4 text-white" />
            </Button>

            {/* Declaration/Engineering toggle */}
            <Button
              variant="outline"
              className={`rounded ${isDeclarationMode ? 'bg-blue-500 hover:bg-blue-600 text-white border-blue-600' : 'bg-white hover:bg-gray-50 border-gray-300'}`}
              onClick={toggleDeclaration}
            >
              {isDeclarationMode ? 'Declaration' : 'Engineering'}
            </Button>

            {/* Status text */}
            {isDeclarationMode ? (
              <span className="text-sm text-gray-700">Ready for declaration 🎉</span>
            ) : (
              <span className="text-sm text-gray-700 flex items-center gap-1">
                <AlertTriangle className="h-4 w-4 text-orange-500" />
                Engineering mode is active
              </span>
            )}
          </div>

          <div className="flex items-center gap-2">
            <DropdownMenu>
              <DropdownMenuTrigger className="inline-flex items-center justify-center gap-1 whitespace-nowrap transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 border border-gray-300 bg-white hover:bg-gray-50 h-9 rounded px-3">
                <Wrench className="h-4 w-4" />
                Tools
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={handleJobEditor}>
                  Job Editor
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleGraph}>
                  Graph
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleOpenLog}>
                  Open Log
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
            
            <Button
              variant="outline"
              className="bg-white hover:bg-gray-50 border-gray-300 rounded gap-1"
              onClick={handleSettings}
            >
              <Settings className="h-4 w-4" />
              Settings
            </Button>
            
            <DropdownMenu>
              <DropdownMenuTrigger className="inline-flex items-center justify-center gap-1 whitespace-nowrap transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 border border-gray-300 bg-white hover:bg-gray-50 h-9 rounded px-3">
                <HelpCircle className="h-4 w-4" />
                Help
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={handleUserManual}>
                  User Manual
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleReleaseNotes}>
                  Release Notes
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleReportIssue}>
                  Report issue via code.europa.eu
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleAbout}>
                  About Vecto
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>

        {/* Main content area */}
        <div className="flex-1 flex gap-4 min-h-0">
          {/* File list */}
          <div className="flex-1 overflow-hidden">
            <FileList
              files={files}
              selectedFiles={selectedFiles}
              activeFileId={activeFileId}
              onFileSelect={setActiveFileId}
              onCheckboxToggle={handleCheckboxToggle}
              onMoveUp={handleMoveUp}
              onMoveDown={handleMoveDown}
              onDelete={handleDelete}
            />
          </div>

          {/* File details */}
          <div className="flex-1 overflow-hidden">
            <FileDetails file={activeFile} />
          </div>
        </div>

        {/* Error panel */}
        <div className="h-32">
          <ErrorPanel errors={errors} onClearError={clearError} />
        </div>

        {/* Footer with progress */}
        <div className="bg-gray-100 border border-gray-300 rounded p-2 flex items-center justify-between">
          <div className="text-sm text-gray-600">
            {isExecuting ? (
              <span>Execution in progress: {progress}%</span>
            ) : (
              <span>Progress: {progress}%</span>
            )}
          </div>
        </div>
      </div>
      <Toaster />
    </div>
  );
}
