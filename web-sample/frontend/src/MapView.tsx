import { useEffect, useRef } from "react";
import L from "leaflet";
import markerIcon2x from "leaflet/dist/images/marker-icon-2x.png";
import markerIcon from "leaflet/dist/images/marker-icon.png";
import markerShadow from "leaflet/dist/images/marker-shadow.png";
import "leaflet/dist/leaflet.css";
import type { GnssPosition } from "./types";

const defaultIcon = L.icon({
  iconUrl: markerIcon,
  iconRetinaUrl: markerIcon2x,
  shadowUrl: markerShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

L.Marker.prototype.options.icon = defaultIcon;

const DEFAULT_CENTER: L.LatLngExpression = [37.7749, -122.4194];
const DEFAULT_ZOOM = 13;

interface MapViewProps {
  position: GnssPosition | null;
}

export default function MapView({ position }: MapViewProps) {
  const mapContainerRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<L.Map | null>(null);
  const markerRef = useRef<L.Marker | null>(null);
  const hasCenteredRef = useRef(false);

  useEffect(() => {
    if (!mapContainerRef.current || mapRef.current) {
      return;
    }

    const map = L.map(mapContainerRef.current, {
      center: DEFAULT_CENTER,
      zoom: DEFAULT_ZOOM,
    });

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
      maxZoom: 19,
    }).addTo(map);

    mapRef.current = map;

    return () => {
      map.remove();
      mapRef.current = null;
      markerRef.current = null;
      hasCenteredRef.current = false;
    };
  }, []);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) {
      return;
    }

    if (!position) {
      if (markerRef.current) {
        markerRef.current.remove();
        markerRef.current = null;
      }
      hasCenteredRef.current = false;
      return;
    }

    const latLng: L.LatLngExpression = [position.latitude, position.longitude];

    if (markerRef.current) {
      markerRef.current.setLatLng(latLng);
    } else {
      markerRef.current = L.marker(latLng).addTo(map);
    }

    if (!hasCenteredRef.current) {
      map.setView(latLng, 18);
      hasCenteredRef.current = true;
    }
  }, [position]);

  return <div ref={mapContainerRef} className="map-container" />;
}
