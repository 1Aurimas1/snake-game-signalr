import { useState } from "react";

export function useLocalStorage(
  keyName: string,
  defaultValue: string | null,
): [string, (newValue: string) => void] {
  const [storedValue, setStoredValue] = useState(() => {
    try {
      const item = window.sessionStorage.getItem(keyName);
      if (item) {
        return JSON.parse(item);
      } else {
        window.sessionStorage.setItem(keyName, JSON.stringify(defaultValue));
        return defaultValue;
      }
    } catch (err) {
      return defaultValue;
    }
  });

  function setValue(newValue: string) {
    try {
      setStoredValue(newValue);
      window.sessionStorage.setItem(keyName, JSON.stringify(newValue));
    } catch (err) {
      console.log(err);
    }
  }

  return [storedValue, setValue];
}
