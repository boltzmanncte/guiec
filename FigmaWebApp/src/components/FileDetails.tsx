import { FileItem } from "./FileList";

interface FileDetailsProps {
  file: FileItem | null;
}

export function FileDetails({ file }: FileDetailsProps) {
  if (!file) {
    return (
      <div className="bg-white border border-gray-300 rounded-lg h-full flex items-center justify-center text-gray-400">
        <p>Select a file to view details</p>
      </div>
    );
  }

  return (
    <div className="bg-white border border-gray-300 rounded-lg h-full p-6">
      <div className="space-y-6">
        <div>
          <h2 className="text-xl mb-1">{file.name}</h2>
          <p className="text-gray-500 text-sm">{file.extension.toUpperCase()}</p>
        </div>
        
        <div className="space-y-4">
          <div className="flex items-start gap-3">
            <div className="text-gray-400 w-24 flex-shrink-0">Size</div>
            <div>{file.size}</div>
          </div>
          
          <div className="flex items-start gap-3">
            <div className="text-gray-400 w-24 flex-shrink-0">Modified</div>
            <div>{file.modified}</div>
          </div>
          
          <div className="flex items-start gap-3">
            <div className="text-gray-400 w-24 flex-shrink-0">Created</div>
            <div>Oct 8, 2025</div>
          </div>
          
          <div className="flex items-start gap-3">
            <div className="text-gray-400 w-24 flex-shrink-0">Location</div>
            <div>/{file.name}</div>
          </div>
        </div>
      </div>
    </div>
  );
}
