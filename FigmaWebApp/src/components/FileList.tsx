import { Checkbox } from "./ui/checkbox";
import { ScrollArea } from "./ui/scroll-area";
import { FileText, ChevronUp, ChevronDown, Trash2 } from "lucide-react";
import { Button } from "./ui/button";

export interface FileItem {
  id: string;
  name: string;
  extension: 'xml' | 'json';
  description: string;
  size: string;
  modified: string;
}

interface FileListProps {
  files: FileItem[];
  selectedFiles: Set<string>;
  activeFileId: string | null;
  onFileSelect: (fileId: string) => void;
  onCheckboxToggle: (fileId: string) => void;
  onMoveUp: () => void;
  onMoveDown: () => void;
  onDelete: () => void;
}

export function FileList({ 
  files, 
  selectedFiles, 
  activeFileId, 
  onFileSelect, 
  onCheckboxToggle,
  onMoveUp,
  onMoveDown,
  onDelete
}: FileListProps) {
  return (
    <div className="bg-white border border-gray-300 rounded-lg h-full flex flex-col">
      <div className="flex-1 overflow-hidden p-4">
        <ScrollArea className="h-full">
          <div className="space-y-1">
            {files.map((file) => (
              <div
                key={file.id}
                className={`flex items-center gap-3 p-3 rounded cursor-pointer transition-colors ${
                  activeFileId === file.id 
                    ? 'bg-[#E6F2FF]' 
                    : 'hover:bg-gray-50'
                }`}
                onClick={() => onFileSelect(file.id)}
              >
                <Checkbox
                  checked={selectedFiles.has(file.id)}
                  onCheckedChange={() => onCheckboxToggle(file.id)}
                  onClick={(e) => e.stopPropagation()}
                  className="data-[state=checked]:bg-[#003399] data-[state=checked]:border-[#003399]"
                />
                
                <FileText 
                  className={`h-5 w-5 ${
                    file.extension === 'xml' ? 'text-orange-500' : 'text-blue-500'
                  }`}
                />
                
                <div className="flex-1">
                  <div>{file.name}</div>
                </div>
                
                <div className="text-gray-500 text-sm">
                  {file.size}
                </div>
              </div>
            ))}
          </div>
        </ScrollArea>
      </div>
      
      {/* Footer with move buttons */}
      <div className="border-t border-gray-300 p-3 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            className="bg-white hover:bg-gray-50 border-gray-300 rounded"
            onClick={onMoveUp}
            disabled={selectedFiles.size !== 1}
          >
            <ChevronUp className="h-4 w-4 mr-1" />
            Move Up
          </Button>
          
          <Button
            variant="outline"
            size="sm"
            className="bg-white hover:bg-gray-50 border-gray-300 rounded"
            onClick={onMoveDown}
            disabled={selectedFiles.size !== 1}
          >
            <ChevronDown className="h-4 w-4 mr-1" />
            Move Down
          </Button>
        </div>
        
        <Button
          variant="outline"
          size="sm"
          className="bg-red-500 hover:bg-red-600 text-white border-red-600 rounded"
          onClick={onDelete}
          disabled={selectedFiles.size === 0}
        >
          <Trash2 className="h-4 w-4" />
          {selectedFiles.size > 0 && ` (${selectedFiles.size})`}
        </Button>
      </div>
    </div>
  );
}
