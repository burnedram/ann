% The lines drawn in this graph is not entirely representative of what the
% neural network actually calculates, as these lines are only from the
% input layer to the hidden layer. This mean that some lines are not
% considered at all by the output layer, or that they are rotated/shifted.

function [A, B, C] = Task4bGrapher(fileTemplate, errorRateFile)

fileID = fopen(errorRateFile);
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%d %f %f'; %neurons trainingError validationError
ErrorRates = fscanf(fileID, spec, [3 Inf])';
fclose(fileID);

fileID = fopen('C:\\ann\\train_data_2016.txt', 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%f %f %f'; %x y class
A = fscanf(fileID, spec, [3 Inf])';
fclose(fileID);

fileID = fopen('C:\\ann\\valid_data_2016.txt', 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%f %f %d'; %x y class
B = fscanf(fileID, spec, [3 Inf])';
fclose(fileID);

C = [A; B];
C(:, 1) = C(:, 1) - mean(C(:, 1));
C(:, 2) = C(:, 2) - mean(C(:, 2));
C(:, 1) = C(:, 1) ./ sqrt(var(C(:, 1)));
C(:, 2) = C(:, 2) ./ sqrt(var(C(:, 2)));

A(:, 1:2) = C(1:size(A, 1), 1:2);
B(:, 1:2) = C((size(A, 1)+1):(size(A, 1)+size(B, 1)), 1:2);

A1 = A(A(:, 3) == 1, :);
Am1 = A(A(:, 3) ~= 1, :);
B1 = B(B(:, 3) == 1, :);
Bm1 = B(B(:, 3) ~= 1, :);

neurons = [0 2 4 8 16 32];
for iNeuron = 1:length(neurons)
    file = sprintf(fileTemplate, neurons(iNeuron));
    fileID = fopen(file);
    if fileID == -1
        disp('Unable to open file');
        return
    end
    spec = '%f %f %f'; %w00 w10 b00
    dump = fscanf(fileID, spec, [3 Inf])';
    a = dump(:, 1:2)';
    b = dump(:, 3)';
    fclose(fileID);

    [path, name, ~] = fileparts(file);
    outfile = sprintf('%s.png', name);
    outname = outfile;
    if path ~= ''
        outfile = strcat([path, filesep, outfile]);
    end
    
    h = figure('Name', outname, 'NumberTitle', 'off');
    hold on;
    scatter(A1(:, 1), A1(:, 2), 'or');
    scatter(Am1(:, 1), Am1(:, 2), 'ob');
    scatter(B1(:, 1), B1(:, 2), 'xr');
    scatter(Bm1(:, 1), Bm1(:, 2), 'xb');
    savey = ylim;

    for iLine = 1:size(dump, 1)
        k = a(1, iLine)/(-a(2, iLine));
        m = sign(-a(2, iLine)) * b(1, iLine)/norm(a(:, iLine));
        plot([-2, 2], [k*(-2) + m, k*2 + m]);
    end
    
    ylim(savey);
    legend({'Training = 1', 'Training = -1', 'Validation = 1', 'Validation = -1'});
    title(sprintf('N = %d, Training_{error} = %1.4f, Validation_{error} = %1.4f', ErrorRates(iNeuron, 1), ErrorRates(iNeuron, 2), ErrorRates(iNeuron, 3)));
    saveas(h, outfile);
end

end