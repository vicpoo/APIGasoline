// PRUEBA DE CARGA: uso normal esperado. Sube a 50 usuarios, se mantiene 5 min y baja.
// Pregunta que responde: ¿la API cumple los tiempos con la carga del día a día?
// k6 run pruebas-carga/2-carga.js
import { setup as setupComun, sesionDeUsuario, umbrales } from './comun.js';

export const options = {
  stages: [
    { duration: '1m', target: 50 },   // subida gradual
    { duration: '5m', target: 50 },   // carga sostenida
    { duration: '1m', target: 0 },    // bajada
  ],
  thresholds: umbrales,
};

export const setup = setupComun;
export default sesionDeUsuario;
