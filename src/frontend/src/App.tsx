// frontend/src/App.tsx
import { useEffect, useState } from 'react';
import { Stage, Layer, Circle, Text, Group, Line, Rect } from 'react-konva';
import type { Unit, Position } from './types/game';

const PPI = 50; // 1 pouce = 50 pixels

function App() {
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [selectionRect, setSelectionRect] = useState<{x1: number, y1: number, x2: number, y2: number} | null>(null);
  const [isSelecting, setIsSelecting] = useState(false);

  // État brouillon : stocke les positions modifiées (en pouces)
  const [movedPositions, setMovedPositions] = useState<Record<string, Position>>({});

  useEffect(() => {
    fetch(`${import.meta.env.VITE_API_URL}/api/game/units`) 
      .then((res) => {
        if (!res.ok) throw new Error('Erreur réseau');
        return res.json();
      })
      .then((data) => {
        setUnits(data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  const handleValidateMovement = async (unitId: string) => {
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/api/game/units/${unitId}/move`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(movedPositions),
      });

      const result = await response.json();

      if (!response.ok) {
        alert(`Refusé par le moteur de jeu : ${result.error}`);
        setMovedPositions({});
      } else {
        alert('Mouvement validé et enregistré !');
        setUnits(units.map(u => u.id === unitId ? result : u));
        setMovedPositions({});
      }
    } catch {
      alert('Erreur de communication avec le serveur.');
    }
  };

  const moveWholeUnit = (unitId: string, dx: number, dy: number) => {
    const unit = units.find(u => u.id === unitId);
    if (!unit) return;

    const newDraftPositions: Record<string, Position> = { ...movedPositions };
    
    unit.models.forEach(model => {
      const current = movedPositions[model.id] || model.position;
      newDraftPositions[model.id] = { x: current.x + dx, y: current.y + dy };
    });

    setMovedPositions(newDraftPositions);
  };

  if (loading) return <div style={{ padding: 20 }}>Chargement du champ de bataille...</div>;
  if (error) return <div style={{ padding: 20, color: 'red' }}>Erreur : {error}</div>;

  return (
    <div style={{ width: '100vw', height: '100vh', backgroundColor: '#2c3e50', overflow: 'hidden' }}>
      
      {/* Menu UI par-dessus le Canvas */}
      <div style={{ position: 'absolute', top: 10, left: 10, color: 'white', zIndex: 1, backgroundColor: 'rgba(0,0,0,0.5)', padding: 15, borderRadius: 8 }}>
        <h2>Wargame Giriden - Prototype</h2>
        <p>Déplace les figurines à la souris.</p>

        <div style={{ marginBottom: '10px' }}>
          <button onClick={() => moveWholeUnit(units[0].id, 1, 0)}>Avancer 1" →</button>
          <button onClick={() => moveWholeUnit(units[0].id, 0, 1)}>Avancer 1" ↓</button>
        </div>
        
        {Object.keys(movedPositions).length > 0 && (
          <button 
            onClick={() => handleValidateMovement(units[0].id)}
            style={{ padding: '8px 16px', backgroundColor: '#2ecc71', color: 'white', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 'bold' }}
          >
            Valider le mouvement du lot
          </button>
        )}
      </div>

      <Stage 
        width={window.innerWidth} 
        height={window.innerHeight}
        onMouseDown={(e) => {
          if (e.target === e.target.getStage()) {
            setIsSelecting(true);
            const pos = e.target.getStage()?.getPointerPosition();
            if(pos) setSelectionRect({ x1: pos.x, y1: pos.y, x2: pos.x, y2: pos.y });
            setSelectedIds([]); // On vide la sélection si on clique dans le vide
          }
        }}
        onMouseMove={(e) => {
          if (!isSelecting) return;
          const pos = e.target.getStage()?.getPointerPosition();
          if (pos && selectionRect) setSelectionRect({ ...selectionRect, x2: pos.x, y2: pos.y });
        }}
        onMouseUp={() => {
          if (isSelecting && selectionRect) {
            const newSelected: string[] = [];
            units.forEach(u => u.models.forEach(m => {
              const px = m.position.x * PPI;
              const py = m.position.y * PPI;
              if (px >= Math.min(selectionRect.x1, selectionRect.x2) && px <= Math.max(selectionRect.x1, selectionRect.x2) &&
                  py >= Math.min(selectionRect.y1, selectionRect.y2) && py <= Math.max(selectionRect.y1, selectionRect.y2)) {
                newSelected.push(m.id);
              }
            }));
            setSelectedIds(newSelected);
          }
          setIsSelecting(false);
          setSelectionRect(null);
        }}
      >
        <Layer>
          {/* 1. Rectangle de sélection (Bleu transparent) */}
          {isSelecting && selectionRect && (
            <Rect
              x={Math.min(selectionRect.x1, selectionRect.x2)}
              y={Math.min(selectionRect.y1, selectionRect.y2)}
              width={Math.abs(selectionRect.x2 - selectionRect.x1)}
              height={Math.abs(selectionRect.y2 - selectionRect.y1)}
              fill="rgba(52, 152, 219, 0.3)"
              stroke="#3498db"
            />
          )}

          {/* 2. Lignes de cohésion */}
          {units.map(unit => 
            unit.models.map((m1, i) => 
              unit.models.slice(i + 1).map((m2) => {
                const pos1 = movedPositions[m1.id] || m1.position;
                const pos2 = movedPositions[m2.id] || m2.position;
                
                const dist = Math.sqrt(Math.pow(pos1.x - pos2.x, 2) + Math.pow(pos1.y - pos2.y, 2));
                const isCohesive = dist <= (2.0 + unit.baseSizeInches);
                
                return (
                  <Line
                    key={`${m1.id}-${m2.id}`}
                    points={[pos1.x * PPI, pos1.y * PPI, pos2.x * PPI, pos2.y * PPI]}
                    stroke={isCohesive ? "#2ecc71" : "#e74c3c"}
                    strokeWidth={2}
                    dash={[5, 5]}
                  />
                );
              })
            )
          )}

          {/* 3. Bulles de portée de mouvement (FIXES à la position d'origine) */}
          {units.map((unit) => 
            unit.models.map((model) => {
              const isSelected = selectedIds.includes(model.id);
              return isSelected ? (
                <Circle
                  key={`range-${model.id}`}
                  x={model.position.x * PPI} 
                  y={model.position.y * PPI} 
                  radius={unit.profile.movement * PPI}
                  stroke="#f1c40f"
                  strokeWidth={2}
                  dash={[10, 5]}
                  listening={false} 
                />
              ) : null;
            })
          )}
          
          {/* 4. Les Figurines (Groupes Draggables) */}
          {units.map((unit) => (
            <Group key={unit.id}>
              {unit.models.map((model) => {
                const currentInches = movedPositions[model.id] || model.position;
                const isSelected = selectedIds.includes(model.id);
                const pixelX = currentInches.x * PPI;
                const pixelY = currentInches.y * PPI;
                const pixelRadius = (unit.baseSizeInches / 2) * PPI;

                return (
                  <Group 
                    key={model.id} 
                    x={pixelX} 
                    y={pixelY}
                    draggable={true}
                    onClick={(e) => {
                      e.cancelBubble = true;
                      if (e.evt.shiftKey) {
                        setSelectedIds(prev => prev.includes(model.id) ? prev.filter(id => id !== model.id) : [...prev, model.id]);
                      } else {
                        setSelectedIds([model.id]);
                      }
                    }}
                    onDragEnd={(e) => {
                      const deltaX = (e.target.x() - (model.position.x * PPI)) / PPI;
                      const deltaY = (e.target.y() - (model.position.y * PPI)) / PPI;

                      const newDraft = { ...movedPositions };

                      if (selectedIds.includes(model.id)) {
                        selectedIds.forEach(id => {
                          const m = units.flatMap(u => u.models).find(m => m.id === id)!;
                          newDraft[id] = { 
                            x: m.position.x + deltaX, 
                            y: m.position.y + deltaY 
                          };
                        });
                      } else {
                        newDraft[model.id] = { 
                          x: model.position.x + deltaX, 
                          y: model.position.y + deltaY 
                        };
                      }
                      setMovedPositions(newDraft);
                    }}
                  >
                    {/* Le socle */}
                    <Circle
                      radius={pixelRadius}
                      fill={isSelected ? "#f1c40f" : (movedPositions[model.id] ? "#3498db" : "#e74c3c")}
                      stroke={isSelected ? "#f39c12" : (movedPositions[model.id] ? "#2980b9" : "#c0392b")}
                      strokeWidth={2}
                    />
                    
                    {/* Le label */}
                    <Text
                      text={model.name.split(' ')[1]}
                      y={-pixelRadius - 15}
                      x={-20}
                      fill="white"
                      fontSize={12}
                    />
                  </Group>
                );
              })}
            </Group>
          ))}
        </Layer>
      </Stage>
    </div>
  );
}

export default App;