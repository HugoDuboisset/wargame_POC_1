export interface Position {
  x: number;
  y: number;
}

export interface UnitProfile {
  movement: number;
  shooting: number;
  combat: number;
  initiative: number;
  morale: number;
  armorClass: number;
}

export interface Model {
  id: string;
  name: string;
  maxHp: number;
  currentHp: number;
  position: Position;
}

export interface Unit {
  id: string;
  name: string;
  faction: string;
  pointsCost: number;
  baseSizeInches: number;
  profile: UnitProfile;
  models: Model[];
}