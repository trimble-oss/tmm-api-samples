export interface RsaPublicJwk {
  kty: string;
  n: string;
  e: string;
}

export interface ReceiverInfo {
  isReceiverConfigured: boolean;
  isConnected: boolean;
  bluetoothName: string | null;
  bluetoothAddress: string | null;
  receiverBrand: string | null;
  receiverModel: string | null;
  receiverSerialNumber: string | null;
  isSigninRequired: boolean;
  isSignedIn: boolean;
}

export interface LocationV2DataMessage {
  latitude: number | null;
  longitude: number | null;
  altitude: number | null;
  speed: number | null;
  bearing: number | null;
  solutionType: string | null;
  hrms: number | null;
  vrms: number | null;
}

export interface GnssPosition {
  latitude: number;
  longitude: number;
}
