// PRUEBA DE HUMO: 1 usuario durante 30 s. Verifica que todo responde antes de meter carga.
// k6 run pruebas-carga/1-humo.js
import { setup as setupComun, sesionDeUsuario, umbrales } from './comun.js';

export const options = {
  vus: 1,
  duration: '30s',
  thresholds: umbrales,
};

export const setup = setupComun;
export default sesionDeUsuario;
