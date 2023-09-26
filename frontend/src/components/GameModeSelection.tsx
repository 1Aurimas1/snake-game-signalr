const GameModeSelection = () => {
  return (
    <div className="flex justify-center items-center min-h-screen">
    <div className="flex flex-col justify-center items-center border">
      <h1 className="p-5">Select game mode</h1>
      <button className="w-28 bg-gray-400 hover:bg-gray-300 font-bold py-2 px-4 rounded">Practice</button>
      <button>Multiplayer</button>
       {[
    ['Home', '/dashboard'],
    ['Team', '/team'],
    ['Projects', '/projects'],
    ['Reports', '/reports'],
  ].map(([title]) => (
    <button className="w-28 bg-slate-400 hover:bg-gray-300 font-bold py-2 px-4 rounded">{title}</a>
  ))}
    </div>
    </div>
  );
};

export default GameModeSelection;
