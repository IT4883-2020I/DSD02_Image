const fs = require('fs');
const exec = require('child_process').exec;
const uuid = require('uuid');
const path = require('path');
const { startStream, endStream } = require('../helpers/storage');
const videoFileExtension = '.webm';
const audioFileExtension = '.wav';
const filePath = './uploads/';

function writeOrAppendData (data, fileName, fileType, videoCounter, ws) {
  if (!fs.existsSync(filePath + fileName + fileType)) {
    console.log('writing original file');
    ws.send(JSON.stringify({ fileName: fileName }));
    fs.writeFileSync(filePath + fileName + fileType, data);
    ws.send(JSON.stringify({ part: videoCounter, fileName: fileName }));
  } else {
    console.log('appending File');
    fs.appendFileSync(filePath + fileName + fileType, data);
    ws.send(JSON.stringify({ part: videoCounter, fileName: fileName }));
  }
}

function fixWebmAudio (fileName, callback) {
  const file = filePath + fileName + videoFileExtension;
  const audioFile = filePath + fileName + audioFileExtension;
  const ffmpegcommand = 'ffmpeg -i ' + file + ' -c:a pcm_s16le ' + audioFile;

  exec(
    ffmpegcommand,
    { maxBuffer: 20000 * 1024 },
    function (error, stdout, stderr) {
      if (error) {
        console.log(error);
      } else {
        callback();
      }
    }
  );
}

function concatVideo (fileNames) {
  console.log('trying to concat videoes');
  const ffmpegcommand =
    'ffmpeg -i ' +
    filePath +
    fileNames[0] +
    videoFileExtension +
    ' -i ' +
    filePath +
    fileNames[1] +
    videoFileExtension +
    ' -i ' +
    filePath +
    fileNames[0] +
    audioFileExtension +
    ' -i ' +
    filePath +
    fileNames[1] +
    audioFileExtension +
    ' -filter_complex "[0:v]scale=iw/2:ih/2,pad=2*iw:ih[left];[1:v]scale=iw/2:ih/2[right];[left][right]overlay=w[out];[2:a][3:a]amerge=inputs=2[a]" -map "[out]" -map "[a]" ' +
    filePath +
    'concated-videos' +
    videoFileExtension;
  console.log(ffmpegcommand);
  exec(
    ffmpegcommand,
    { maxBuffer: 20000 * 1024 },
    function (error, stdout, stderr) {
      if (error) {
        console.log(error);
      } else {
        console.log('concat finished');
      }
    }
  );
}

module.exports = function (app) {
  const sockets = [];
  const conferences = {};
  let sessionDescription;
  let fileName;

  function broadcast (data) {
    console.log('trying to broadcast data');
    sockets.forEach(function (socket) {
      if (socket.readyState === 1) {
        socket.send(data);
      }
    });
  }

  function allVideosRecorded (id) {
    let allRecorded = true;
    conferences[id].forEach(function (file) {
      const fileName = Object.keys(file)[0];
      if (file[fileName] === false) {
        allRecorded = false;
      }
    });
    return allRecorded;
  }

  function compileConferenceVideos (id) {
    let counter = 0;
    const fileNames = [];
    function callback () {
      counter += 1;
      console.log('compile conferece counter ' + counter);
      if (counter === 2) {
        concatVideo(fileNames);
      }
    }
    conferences[id].forEach(function (file) {
      const fileName = Object.keys(file)[0];
      fileNames.push(fileName);
      fixWebmAudio(fileName, callback);
    });
  }

  app.ws('/', function (ws, req) {
    sockets.push(ws);
    console.log('new connection established');
    let videoCounter = 0;
    const fileType = videoFileExtension;

    if (req._parsedUrl.query === 'type=stream') {
      fileName = uuid.v1();
      ws.on('message', async function (pureData) {
        if (pureData instanceof Buffer) {
          console.log('got binary data');
          videoCounter++;
          writeOrAppendData(pureData, fileName, fileType, videoCounter, ws);
        } else {
          const data = JSON.parse(pureData);
          console.log(data);
          if (data.id && !data.completedVideo) {
            const conferenceID = data.id;
            const conferencePair = conferences[conferenceID];
            console.log(conferencePair);
            if (!conferencePair) {
              const obj = {};
              obj[fileName] = false;
              conferences[conferenceID] = [obj];
            } else {
              const obj = {};
              obj[fileName] = false;
              conferencePair.push(obj);
            }
          } else if (data.id && data.completedVideo) {
            console.log('Complete Video', data);
            const videoPath = path.join(
              __dirname,
              '../uploads',
              `${data.completedVideo}${videoFileExtension}`
            );
            const src = `http://192.168.1.102:7002/stream/download?src=${videoPath}`;
            await endStream(fileName, new Date().getTime(), src, null);
            broadcast(
              JSON.stringify({
                type: 'completed',
                link: src
              })
            );
            if (conferences[data.id]) {
              conferences[data.id].forEach(function (participant) {
                if (participant[fileName] === false) {
                  participant[fileName] = true;
                }
              });
              console.log(conferences);
              if (allVideosRecorded(data.id)) {
                console.log('trying to compile video');
                compileConferenceVideos(data.id);
              }
            }
          } else {
            const { type, sessionDescription: ssDes } = data;
            if (type === 'offer') {
              sessionDescription = ssDes;
              const videoPath = path.join(
                __dirname,
                '../uploads',
                `${fileName}${videoFileExtension}`
              );

              const src = `http://192.168.1.102:7002/stream/download?src=${videoPath}`;

              await startStream(
                fileName,
                new Date().getTime(),
                ssDes,
                src,
                null
              );
            }
            broadcast(JSON.stringify(data));
          }
        }
      });
    } else {
      ws.on('message', function (pureData) {
        const { type } = JSON.parse(pureData);
        if (type === 'offer_watch' && sessionDescription) {
          console.log(1111, fileName);
          broadcast(
            JSON.stringify({
              type: 'answer_watch',
              sessionDescription,
              fileName
            })
          );
        } else {
          broadcast(pureData);
        }
      });
    }
  });
};
