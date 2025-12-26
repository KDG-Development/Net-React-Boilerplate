import { useState } from "react";
import { TextInput, ActionButton } from "kdg-react";

type ControlledSearchInputProps = {
  value: string | null;
  onSearch: (term: string | null) => void;
  placeholder?: string;
};

export const ControlledSearchInput = (props: ControlledSearchInputProps) => {
  const [localValue, setLocalValue] = useState<string | null>(props.value);

  const handleSearch = (value: string | null) => {
    const trimmed = value?.trim() || null;
    props.onSearch(trimmed);
  };

  const handleChange = (value: string | null) => {
    setLocalValue(value);
    // Trigger search on clear (native clear button or empty value)
    if (!value) {
      handleSearch(null);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch(localValue);
    }
  };

  return (
    <div className="d-flex align-items-center gap-1" onKeyDown={handleKeyDown}>
      <div className="flex-grow-1 d-flex align-items-center gap-1">
        <TextInput
          placeholder={props.placeholder ?? "Search..."}
          value={localValue}
          onChange={handleChange}
          type="search"
        />
        <ActionButton
          onClick={() => handleSearch(localValue)}
          variant="outline"
        >
          Search
        </ActionButton>
      </div>
    </div>
  );
};

