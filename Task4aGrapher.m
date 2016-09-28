function [A, B, C] = Hagge(a, b)
fileID = fopen('C:\\ann\\train_data_2016.txt', 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%f %f %d'; %x y class
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

k = a(1)/(-a(2));
m = b(1)/norm(a);

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

errorsA = 0;
for i = 1:size(A, 1)
    clazz = tanh((A(i, 1:2) * a + b) / 2);
    if clazz >= 0
        clazz = 1;
    else
        clazz = -1;
    end
    if clazz ~= A(i, 3)
        errorsA = errorsA + 1;
    end
end
errorRateA = errorsA / size(A, 1);

errorsB = 0;
for i = 1:size(B, 1)
    clazz = tanh((B(i, 1:2) * a + b) / 2);
    if clazz >= 0
        clazz = 1;
    else
        clazz = -1;
    end
    if clazz ~= B(i, 3)
        errorsB = errorsB + 1;
    end
end
errorRateB = errorsB / size(B, 1);

figure;
hold on;
scatter(A1(:, 1), A1(:, 2), 'or');
scatter(Am1(:, 1), Am1(:, 2), 'ob');
scatter(B1(:, 1), B1(:, 2), 'xr');
scatter(Bm1(:, 1), Bm1(:, 2), 'xb');
savey = ylim;
plot([-2, 2], [k*(-2) + m, k*2 + m]);
plot([-2, 2], [k*(-2) - m, k*2 - m]);
ylim(savey);
legend({'Training = 1', 'Training = -1', 'Validation = 1', 'Validation = -1', 'y = k*x + m', 'y = k*x - m'});
title(sprintf('Training_{error} = %1.4f, Validation_{error} = %1.4f', errorRateA, errorRateB));
end