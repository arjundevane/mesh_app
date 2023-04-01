import React from 'react';
import './App.css';

function App() {
  return (
    <div className="App">
      <h2>This is the root.</h2>
      <SampleButton
        displayText="Show Me The Money"
      ></SampleButton>
    </div>
  );
}

function SampleButton({
  displayText
}: { displayText: string }) {
  return (
    <button>{displayText}</button>
  );
}

export default App;
