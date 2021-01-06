const axios = require('axios');

const ENDPOINT = 'https://it4483team2.herokuapp.com/api';

const startStream = async (
  streamId,
  startTime,
  sessionDescription,
  link,
  metaData
) => {
  try {
    const { data } = await axios.post(`${ENDPOINT}/streams/start-stream`, {
      streamId,
      startTime,
      sessionDescription,
      link,
      metaData
    });
    return { success: true, data };
  } catch (error) {
    return { success: false, error };
  }
};
const endStream = async (streamId, stopTime, link, metaData) => {
  try {
    const { data } = await axios.post(
      `${ENDPOINT}/streams/stop-stream/${streamId}`,
      {
        streamId,
        stopTime,
        link,
        metaData
      }
    );
    return { success: true, data };
  } catch (error) {
    return { success: false, error };
  }
};
const cropStream = async (streamId, start, link, duration) => {
  try {
    const { data } = await axios.post(
      `${ENDPOINT}/records/cut-stream/${streamId}`,
      {
        link,
        videoId: streamId,
        start,
        duration
      }
    );
    return { success: true, data };
  } catch (error) {
    return { success: false, error };
  }
};
const cropVideo = async (streamId, start, link, duration) => {
  try {
    const { data } = await axios.post(
      `${ENDPOINT}/records/cut-video/${streamId}`,
      {
        link,
        videoId: streamId,
        start,
        duration
      }
    );
    return { success: true, data };
  } catch (error) {
    return { success: false, error };
  }
};

module.exports = {
  startStream,
  endStream,
  cropStream,
  cropVideo
};
