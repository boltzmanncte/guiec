import { Alert, AlertDescription } from "./ui/alert";
import { AlertCircle, X } from "lucide-react";
import { Button } from "./ui/button";

interface ErrorPanelProps {
  errors: string[];
  onClearError: (index: number) => void;
}

export function ErrorPanel({ errors, onClearError }: ErrorPanelProps) {
  if (errors.length === 0) {
    return (
      <div className="bg-white border border-gray-300 rounded-lg p-4 h-full flex items-center justify-center text-gray-400 text-sm">
        No errors
      </div>
    );
  }

  return (
    <div className="bg-white border border-gray-300 rounded-lg p-4 h-full overflow-auto">
      <div className="space-y-2">
        {errors.map((error, index) => (
          <Alert key={index} variant="destructive" className="relative pr-12 bg-white">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{error}</AlertDescription>
            <Button
              variant="ghost"
              size="sm"
              className="absolute right-2 top-2 h-6 w-6 p-0"
              onClick={() => onClearError(index)}
            >
              <X className="h-4 w-4" />
            </Button>
          </Alert>
        ))}
      </div>
    </div>
  );
}
